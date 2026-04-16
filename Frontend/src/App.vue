<script setup lang="ts">
import { onMounted, ref, computed, onUnmounted, watch } from 'vue'
import { initMap } from './scripts/maps/map.ts'
import {
  clearAllPolygons,
  fetchAllNeighborhoods,
  type NeighborhoodInfo,
  neighborhoodOutlines,
} from './scripts/maps/neighborhoodMap.ts'
import { registrarContaCPF } from './scripts/user/userCPF.ts'
import { registrarContaCNPJ } from './scripts/user/userCNPJ.ts'
import type {
  UserGeneric,
  UserCPF,
  UserCNPJ,
  UserLogin,
  UserAccount,
} from './scripts/user/userGeneric.ts'
import { giveAccountInfo } from './scripts/user/authLogin.ts'

//Variaveis de teste
const city = ref<string>('Porto Alegre')
const neighborhoodsNoPower = ref<string[]>([])

//Inicialização do mapa
const initiateMap = ref<google.maps.Map | undefined>(undefined)

//Variaveis do menu
const openMenu = ref(true)
const openChat = ref(true)
const newMessage = ref('')
const activeTab = ref('chat')
const messages = ref([{ user: 'Test', text: 'Mensagem de teste.' }])
const selectedTab = (tabName: string) => {
  activeTab.value = activeTab.value === tabName ? 'chat' : tabName
}
const sendMessage = () => {
  if (newMessage.value.trim()) {
    messages.value.push({
      user: 'Usuario',
      text: newMessage.value,
    })
    newMessage.value = ''

    setTimeout(() => {
      const chatContainer = document.querySelector('.box-chat-messages')
      if (chatContainer) chatContainer.scrollTop = chatContainer.scrollHeight
    }, 50)
  }
}
const onlineUsers = ref([
  { name: 'Visitante_Alpha', location: 'Bela Vista', status: 'Online' },
  { name: 'Visitante_Beta', location: 'Centro Histórico', status: 'Online' },
])

//Variaveis para reporte
const neighborhoodsList = ref<NeighborhoodInfo[]>([])
const detectLocation = ref('')
const putManualLocation = ref('')
const isChangingReport = ref(false)
const searchReportQuery = ref('')
const displayNeighborhood = computed(
  () => putManualLocation.value || detectLocation.value || 'Detectando...',
)
const filteredNeighborhoods = computed(() =>
  neighborhoodsList.value.filter((n) =>
    n.name.toLowerCase().includes(searchReportQuery.value.toLocaleLowerCase()),
  ),
)
const handleDetected = (e: any) => (detectLocation.value = e.detail.name)
const handleReport = async () => {
  console.log(`Enviado para a API o reporte: ${displayNeighborhood.value}`)
  const reportedNeighborhood = displayNeighborhood.value

  if (!reportedNeighborhood || reportedNeighborhood === 'Detectando...') return

  if (!neighborhoodsNoPower.value.includes(reportedNeighborhood)) {
    neighborhoodsNoPower.value.push(reportedNeighborhood)

    if (initiateMap.value) {
      await neighborhoodOutlines(initiateMap.value, neighborhoodsNoPower.value, city.value, false)
    }
    console.log(`Reportado o bairro: ${reportedNeighborhood}`)

    putManualLocation.value = ''
  }
}
const selectManual = (name: string) => {
  putManualLocation.value = name
  isChangingReport.value = false
}

const handleLocationDetected = async (e: any) => {
  const { neighborhood: newNeighborhood, city: newCity } = e.detail

  const strictCity = city.value !== ''

  if (!strictCity && newCity && newCity !== city.value) {
    console.warn(`Mudança de cidade realizada: ${city.value} -> ${newCity}`)

    clearAllPolygons()

    localStorage.removeItem(`city-bounds-${city.value}`)
    localStorage.removeItem(`city-outline-${city.value}`)
    localStorage.removeItem(`${city.value}-neighborhoods`)

    city.value = newCity
    neighborhoodsList.value = await fetchAllNeighborhoods(newCity)

    const mapDiv = document.getElementById('map-canvas')
    if (mapDiv) mapDiv.innerHTML = ''

    initiateMap.value = await initMap('map-canvas', newCity, neighborhoodsNoPower.value)

    detectLocation.value = newNeighborhood
  } else if (strictCity) {
    if (newCity === city.value) {
      detectLocation.value = newNeighborhood
    } else {
      console.warn(`Modo Estrito: Ignorada mudança para ${newCity}. Mantendo em ${city.value}`)
      detectLocation.value = 'Fora de area.'
    }
  }
}

const sleep = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms))

const loadNeighborhoodList = async (attempts = 3) => {
  let timer = 5000
  for (let i = 0; i < attempts; i++) {
    try {
      const names = await fetchAllNeighborhoods(city.value)
      if (names.length > 0) {
        neighborhoodsList.value = names
        return
      }
    } catch (e) {
      console.warn(`Tentativa ${i + 1} falhou. Erro: `, e)
    }
    if (i < attempts - 1) {
      console.warn(`Aguardando ${timer / 1000}s para a proxima tentativa.`)
      await sleep(timer)
      timer *= 2
    }
  }
  console.error('Não foi possivel carregar a lista de bairros.')
}

//Variaveis de conta

const currentUser = ref<UserAccount | null>(null)
const loggedUser = ref(false)
const isRegistered = ref(false)
const razaoSocial = ref('CPF')
const selectRegisterNeighborhood = ref(false)

const loginForm = ref({
  nome: '',
  senha: '',
})

const registerForm = ref({
  nome: '',
  razao_social: '',
  telefone: '',
  email: '',
  senha: '',
  data: '',
  bairro_criacao: '',
  bairro_id: 0,
})

const handleLogin = async () => {
  try {
    const { account } = await giveAccountInfo(loginForm.value)

    console.log(
      `dados de loginForm: Nome -> ${loginForm.value.nome}| Senha -> ${loginForm.value.senha}`,
    )

    currentUser.value = account

    loggedUser.value = true
    activeTab.value = 'chat'
    console.log(`Acesso na conta de ${currentUser.value.nome} carregado com sucesso`)

    if (currentUser.value.accountType === 'PersonAccount') {
      console.log('Essa é uma conta pessoal')
    } else if (currentUser.value.accountType === 'BusinessAccount') {
      console.log('Essa é uma conta empresarial')
    } else {
      console.log('A conta não possui nenhum tipo')
    }

    console.log(`Você esta logado na conta de ${currentUser.value.nome}`)
  } catch (e) {
    console.error('Erro ao tentar login: ', e)
  }
}

const handleRegistration = async () => {
  const { nome, razao_social, telefone, email, senha, data, bairro_criacao, bairro_id } =
    registerForm.value

  const baseData: UserGeneric = {
    nome,
    telefone,
    email,
    senha,
    bairro_criacao: bairro_criacao || detectLocation.value,
    bairro_id,
    descricao: '',
    imagem_perfil_link: '',
  }

  try {
    if (razaoSocial.value === 'CPF') {
      const newUser: UserCPF = {
        ...baseData,
        cpf: razao_social,
        data_nascimento: data,
      }

      await registrarContaCPF(newUser)
    } else {
      const newUser: UserCNPJ = {
        ...baseData,
        cnpj: razao_social,
        data_criacao: data,
        slot_anuncio_quantidade: 3,
      }

      await registrarContaCNPJ(newUser)
    }

    loggedUser.value = true
    isRegistered.value = true
  } catch (error) {
    console.error('Falha ao realizar registro:', error)
  }
}

watch(
  detectLocation,
  (newBairro) => {
    if (newBairro && !registerForm.value.bairro_criacao) {
      registerForm.value.bairro_criacao = newBairro
    }
  },
  { immediate: true },
)

const handleLoginStay = () => {
  if (activeTab.value === 'chat') {
    isRegistered.value = true
  }
  selectedTab('profile')
}

const handleInputDropdownClick = () => {
  registerForm.value.bairro_criacao = ''

  selectRegisterNeighborhood.value = !selectRegisterNeighborhood.value
}

const filteredRegisterNeighborhoods = computed(() =>
  neighborhoodsList.value.filter((n) =>
    n.name.toLowerCase().includes(registerForm.value.bairro_criacao.toLowerCase()),
  ),
)

const selectingRegisterNeighborhood = (neighborhood: NeighborhoodInfo) => {
  registerForm.value.bairro_criacao = neighborhood.name
  registerForm.value.bairro_id = neighborhood.id
  selectRegisterNeighborhood.value = false
  InputFix('bairro_criacao')
}

const nameRegex = /(?!.*(\S)\1\1)^\S+( \S+)+/
const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/
const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$/

const verifyErrorRegister = ref({
  nome: false,
  senha: false,
  telefone: false,
  email: false,
  razaoSocial: false,
  data: false,
  bairro_criacao: false,
})

const InputFix = (field: string) => {
  const form = registerForm.value

  if (field === 'nome') verifyErrorRegister.value.nome = !nameRegex.test(form.nome)
  if (field === 'senha') verifyErrorRegister.value.senha = !passwordRegex.test(form.senha)
  if (field === 'email') verifyErrorRegister.value.email = !emailRegex.test(form.email)
  if (field === 'data') verifyErrorRegister.value.data = form.data === ''
  if (field === 'bairro_criacao')
    verifyErrorRegister.value.bairro_criacao = form.bairro_criacao === ''
  if (field === 'razao_social') {
    const len = form.razao_social.replace(/\D/g, '').length
    verifyErrorRegister.value.razaoSocial = razaoSocial.value === 'CPF' ? len !== 11 : len !== 14
  }
  if (field === 'telefone') {
    verifyErrorRegister.value.telefone = form.telefone.replace(/\D/g, '').length !== 11
  }
}
const allIsValidRegister = computed(() => {
  const errors = verifyErrorRegister.value
  const form = registerForm.value

  return (
    !Object.values(errors).some((v) => v === true) && Object.values(form).every((v) => v !== '')
  )
})

onMounted(async () => {
  window.addEventListener('neighborhood-detected', handleDetected)
  window.addEventListener('map-neighborhood-clicked', (e: any) => {
    putManualLocation.value = e.detail.name
    isChangingReport.value = false
    console.log(`Bairro clickado: ${e.detail.name}`)
  })
  window.addEventListener('location-detected', handleLocationDetected)

  //Carregamento da pagina é realizado sequencialmente
  //Evitar bug de chamadas de componentes não dependentes

  try {
    const names = await fetchAllNeighborhoods(city.value)

    neighborhoodsList.value = names

    initiateMap.value = await initMap('map-canvas', city.value, neighborhoodsNoPower.value)

    console.log(`Mapa de ${city.value} foi carregado com ${names.length} bairros.`)
  } catch (error) {
    console.warn('Erro ao realizar carregamento da pagina: ', error)
  }

  if (neighborhoodsList.value.length === 0) {
    loadNeighborhoodList()
  }
})
onUnmounted(() => {
  window.removeEventListener('neighborhood-detected', handleDetected)
  window.removeEventListener('location-detected', handleLocationDetected)
  window.removeEventListener('map-neighborhood-clicked', () => {})
})
</script>

<style lang="scss">
@use './assets/styles/App.scss';
</style>

<template>
  <div class="above-content">
    <div class="box-news"><h1 class="placard-h1">News</h1></div>
  </div>
  <div class="below-content">
    <button v-if="!openMenu" @click="openMenu = true" class="button-power-outage-outside">P</button>
    <button v-if="!openChat" @click="openChat = true" class="button-chat-outside"></button>
    <div class="box-report-wrapper">
      <div class="box-report-card">
        <template v-if="!isChangingReport">
          <p class="box-report-label">Reportar falta de luz:</p>
          <h3 class="box-report-neighborhood">{{ displayNeighborhood }}</h3>
          <button class="box-report-btn-main" @click="handleReport">SEM LUZ IRMÃO</button>
          <button class="box-report-btn-change" @click="isChangingReport = true">
            Mudar bairro
          </button>
        </template>
        <template v-else>
          <input
            v-model="searchReportQuery"
            type="text"
            placeholder="Buscar bairro..."
            class="box-report-input"
          />
          <ul class="box-report-dropdown">
            <li v-for="n in filteredNeighborhoods" :key="n.id" @click="selectManual(n.name)">
              {{ n.name }}
            </li>
          </ul>
          <button class="box-report-btn-change" @click="isChangingReport = false">Cancelar</button>
        </template>
      </div>
    </div>
    <div class="box-power-outage" :class="{ 'is-hidden': !openMenu }">
      <div class="box-power-outage-header">
        <h2 class="box-power-outrage-h2">Bairros sem luz</h2>
        <button class="button-power-outage-inside" @click="openMenu = false">X</button>
      </div>
      <ul class="lista-bairros-sem-luz">
        <li v-if="neighborhoodsNoPower.length === 0" class="lista-items-bairros-sem-luz">
          <strong>Nenhum bairro reportado</strong>
        </li>
        <li v-for="n in neighborhoodsNoPower" :key="n" class="lista-items-bairros-sem-luz">
          <strong>{{ n }}</strong>
        </li>
      </ul>
    </div>
    <div class="box-map" id="map-canvas"><h1>Map</h1></div>
    <div class="box-chat" :class="{ isHidden: !openChat }">
      <div class="box-chat-header">
        <div class="box-chat-verify-logged">
          <div class="box-chat-profile-image" :class="{ 'is-logged': loggedUser }"></div>
          <div class="box-chat-toggle-profile-online">
            <span class="button-switch-profile" @click="handleLoginStay">{{
              activeTab === 'profile'
                ? loggedUser
                  ? 'Meu perfil'
                  : 'Voltar ao chat'
                : 'Entrar / Cadastro'
            }}</span>
            <span
              v-if="loggedUser"
              :class="{ 'is-active': activeTab === 'online' }"
              class="button-switch-online"
              @click="selectedTab('online')"
            >
              {{
                activeTab === 'online' ? 'Fechar Lista' : `${onlineUsers.length} usuarios online`
              }}
            </span>
          </div>
        </div>
        <button class="box-chat-button" @click="openChat = false">X</button>
      </div>
      <div class="box-chat-content">
        <Transition name="slide" mode="out-in">
          <div v-if="activeTab === 'chat'" key="chat" class="box-chat-view-chat">
            <div class="box-chat-messages">
              <div v-for="(msg, index) in messages" :key="index" class="message">
                <strong>{{ msg.user }}: </strong>{{ msg.text }}
              </div>
            </div>
            <div class="box-chat-input">
              <input
                v-model="newMessage"
                type="text"
                placeholder="Digite algo..."
                @keyup.enter="sendMessage"
              />
              <button @click="sendMessage">➤</button>
            </div>
          </div>
          <div v-else-if="activeTab === 'profile'" key="profile" class="box-chat-viewprofile">
            <div v-if="!loggedUser">
              <Transition name="slide" mode="out-in">
                <div v-if="isRegistered" class="box-chat-loginform">
                  <h3>Acessar a conta</h3>
                  <input v-model="loginForm.nome" type="text" placeholder="Nome" />
                  <input v-model="loginForm.senha" type="password" placeholder="Senha" />
                  <button class="box-chat-loginform-button" @click="handleLogin">ENTRAR</button>
                  <div class="box-chat-loginform-verifications">
                    <span class="box-chat-loginform-forgot-password">Esqueceu de sua senha?</span>
                    <span class="box-chat-loginform-not-registered" @click="isRegistered = false"
                      >Não possui registro?</span
                    >
                  </div>
                </div>
                <div v-else class="box-chat-registerform">
                  <h3>Criar sua conta</h3>
                  <div class="box-chat-registerform-typeaccount">
                    <button :class="{ active: razaoSocial === 'CPF' }" @click="razaoSocial = 'CPF'">
                      CPF
                    </button>
                    <button
                      :class="{ active: razaoSocial === 'CNPJ' }"
                      @click="razaoSocial = 'CNPJ'"
                    >
                      CNPJ
                    </button>
                  </div>
                  <div class="box-chat-registerform-inputgroup">
                    <input
                      v-model="registerForm.nome"
                      type="text"
                      :class="{ 'box-chat-registerform-input-error': verifyErrorRegister.nome }"
                      :placeholder="razaoSocial === 'CPF' ? 'Nome Completo' : 'Razão Social'"
                      @blur="InputFix('nome')"
                      required
                    />
                    <span v-if="verifyErrorRegister.nome" class="box-chat-registerform-errormsg"
                      >Insira seu nome completo</span
                    >
                  </div>
                  <div class="box-chat-registerform-inputgroup">
                    <input
                      v-model="registerForm.email"
                      type="text"
                      @blur="InputFix('email')"
                      placeholder="E-mail"
                      required
                    />
                    <span v-if="verifyErrorRegister.email" class="box-chat-registerform-errormsg"
                      >Insira seu email valido</span
                    >
                  </div>
                  <div class="box-chat-registerform-inputgroup">
                    <input
                      v-model="registerForm.senha"
                      type="password"
                      @blur="InputFix('senha')"
                      placeholder="Senha"
                      required
                    />
                    <span v-if="verifyErrorRegister.senha" class="box-chat-registerform-errormsg"
                      >Insira uma senha forte</span
                    >
                  </div>
                  <div class="box-chat-registerform-inputgroup">
                    <input
                      v-model="registerForm.razao_social"
                      type="text"
                      :placeholder="
                        razaoSocial === 'CPF' ? 'CPF(000.000.000-00)' : 'CNPJ(00.000.000/0000-00)'
                      "
                      @blur="InputFix('razao_social')"
                      required
                    />
                    <span
                      v-if="verifyErrorRegister.razaoSocial"
                      class="box-chat-registerform-errormsg"
                      >Insira seu numero de registro valido</span
                    >
                  </div>
                  <div class="box-chat-registerform-inputgroup">
                    <input
                      v-model="registerForm.data"
                      type="date"
                      min="1926-01-01"
                      max="2025-12-31"
                      @blur="InputFix('data')"
                      required
                    />
                    <span v-if="verifyErrorRegister.data" class="box-chat-registerform-errormsg"
                      >Insira uma data</span
                    >
                  </div>
                  <div class="box-chat-registerform-neighborhood" @click="handleInputDropdownClick">
                    <input
                      v-model="registerForm.bairro_criacao"
                      type="text"
                      placeholder="Seu bairro"
                      @blur="InputFix('bairro_criacao')"
                      readonly
                      required
                    />
                    <ul
                      v-if="selectRegisterNeighborhood && filteredRegisterNeighborhoods.length"
                      class="box-chat-registerform-dropdown"
                    >
                      <li
                        v-for="n in filteredNeighborhoods"
                        :key="n.id"
                        @click.stop="selectingRegisterNeighborhood(n)"
                      >
                        {{ n.name }}
                      </li>
                    </ul>
                    <span
                      v-if="verifyErrorRegister.bairro_criacao"
                      class="box-chat-registerform-errormsg"
                      >Insira o bairro de criação da conta</span
                    >
                  </div>
                  <div class="box-chat-registerform-inputgroup">
                    <input
                      v-model="registerForm.telefone"
                      type="tel"
                      placeholder="Seu numero de celular"
                      @blur="InputFix('telefone')"
                      pattern="\(\d{2}\)\s\d{5}-\d{4}"
                      required
                    />
                    <span v-if="verifyErrorRegister.telefone" class="box-chat-registerform-errormsg"
                      >Insira um numero de telefone</span
                    >
                  </div>
                  <button
                    class="box-chat-registerform-btn"
                    :disabled="!allIsValidRegister"
                    @click="handleRegistration"
                  >
                    CADASTRAR
                  </button>
                  <div class="box-chat-loginform-verifications">
                    <span class="box-chat-loginform-not-registered" @click="isRegistered = true"
                      >Já possui conta?</span
                    >
                  </div>
                </div>
              </Transition>
            </div>
            <div v-else class="box-chat-profile-table-container">
              <div
                v-if="currentUser?.accountType === 'PersonAccount'"
                class="box-chat-profile-cpf-container"
              >
                <table class="box-chat-profile-table">
                  <thead>
                    <tr>
                      <th colspan="2">Dados do usuário</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td>Nome:</td>
                      <td>{{ currentUser.nome }}</td>
                    </tr>
                    <tr>
                      <td>Localização:</td>
                      <td>Porto Alegre, RS</td>
                    </tr>
                    <tr>
                      <td>Status:</td>
                      <td><span class="status-online">Online</span></td>
                    </tr>
                    <tr>
                      <td>Notificações:</td>
                      <td>Ativadas</td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <div v-else class="box-chat-profile-cnpj-container">
                <table class="box-chat-profile-table">
                  <thead>
                    <tr>
                      <th colspan="2">Dados do empresa</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td>Nome:</td>
                      <td>{{ currentUser?.nome }}</td>
                    </tr>
                    <tr>
                      <td>Localização:</td>
                      <td>Porto Alegre, RS</td>
                    </tr>
                    <tr>
                      <td>Status:</td>
                      <td><span class="status-online">Online</span></td>
                    </tr>
                    <tr>
                      <td>Notificações:</td>
                      <td>Ativadas</td>
                    </tr>
                  </tbody>
                </table>
              </div>

              <button class="box-chat-viewprofile-logoutbutton" @click="loggedUser = false">
                Sair da conta
              </button>
            </div>
          </div>
          <div v-else-if="activeTab === 'online'" key="online" class="box-chat-viewonline">
            <div class="box-chat-viewonline-container">
              <table class="box-chat-viewonline-table">
                <thead>
                  <tr>
                    <th>Usuário</th>
                    <th>Local</th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="user in onlineUsers" :key="user.name">
                    <td>{{ user.name }}</td>
                    <td>{{ user.location }}</td>
                  </tr>
                </tbody>
              </table>
              <button class="box-chat-viewonline-logoutbutton" @click="selectedTab('chat')">
                Voltar ao chat
              </button>
            </div>
          </div>
        </Transition>
      </div>
    </div>
  </div>
</template>
