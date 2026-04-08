<script setup lang="ts">
import { onMounted, ref, computed, onUnmounted } from 'vue'
import { initMap } from './scripts/maps/map.ts'
import {
  clearAllPolygons,
  fetchAllNeighborhoods,
  neighborhoodOutlines,
} from './scripts/maps/neighborhoodMap.ts'

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
const loggedUser = ref(false)
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
const neighborhoodsList = ref<string[]>([])
const detectLocation = ref('')
const putManualLocation = ref('')
const isChangingReport = ref(false)
const searchReportQuery = ref('')
const displayNeighborhood = computed(
  () => putManualLocation.value || detectLocation.value || 'Detectando...',
)
const filteredNeighborhoods = computed(() =>
  neighborhoodsList.value.filter((n) =>
    n.toLowerCase().includes(searchReportQuery.value.toLocaleLowerCase()),
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
            <li v-for="n in filteredNeighborhoods" :key="n" @click="selectManual(n)">
              {{ n }}
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
            <span class="button-switch-profile" @click="selectedTab('profile')">{{
              activeTab === 'profile'
                ? loggedUser
                  ? 'Meu perfil'
                  : 'Entrar / Cadastro'
                : 'Voltar ao chat'
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
            <div v-if="!loggedUser" class="box-chat-loginform">
              <h3>Acessar a conta</h3>
              <input type="text" placeholder="E-mail" />
              <input type="password" placeholder="Senha" />
              <button class="box-chat-loginform-button" @click="loggedUser = true">ENTRAR</button>
            </div>
            <div v-else class="box-chat-profile-table-container">
              <table class="box-chat-profile-table">
                <thead>
                  <tr>
                    <th colspan="2">Dados do usuário</th>
                  </tr>
                </thead>
                <tbody>
                  <tr>
                    <td>Nome:</td>
                    <td>Visitante Teste</td>
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
