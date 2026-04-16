import { jwtDecode } from 'jwt-decode'
import { type UserAccount, type UserCNPJ, type UserCPF, type UserLogin } from './userGeneric'

const API_BANCO_DE_DADOS = 'http://localhost:5176/accounts'

const fetchToken = async (credentials: UserLogin) => {
  const payload = {
    Username: credentials.nome,
    Password: credentials.senha,
  }

  try {
    const response = await fetch(`${API_BANCO_DE_DADOS}/login`, {
      method: 'POST',
      headers: {
        'Content-type': 'application/json',
      },
      body: JSON.stringify(payload),
    })

    if (!response.ok) {
      throw new Error(`Falha na autenticação de login: ${response.status}`)
    }

    const data = await response.json()

    console.log(data.token)

    localStorage.setItem('userToken', data.token)

    return data.token
  } catch (e) {
    console.error('Erro ao tentar conexão com banco de dados: ', e)
    throw e
  }
}

const verifyLogin = async (token: string) => {
  const decode: any = jwtDecode(token)

  console.log(decode)

  const userId = parseInt(decode.nameid)
  const userRole = decode.role

  return { userId, userRole }
}

export const giveAccountInfo = async (credentials: UserLogin) => {
  const token = await fetchToken(credentials)

  const { userId, userRole } = await verifyLogin(token)

  console.log(`Esse é o token: ${token}, e esse é o userId: ${userId}`)

  try {
    const response = await fetch(`${API_BANCO_DE_DADOS}/${userId}`, {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${token}`,
        Accept: 'application/json',
      },
    })

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error('Sessão expirada.')
      }
      throw new Error(`Erro ao tentar dar GET nas informações de usuario: ${response.status}`)
    }

    const accountData = await response.json()
    console.log(accountData)

    let account: UserAccount

    if (userRole === 'PersonAccount') {
      const publicData = accountData.person_Account_Data.public_Data
      account = {
        nome: publicData.username,
        email: publicData.email,
        telefone: '',
        data_nascimento: publicData.birthday,
        bairro_criacao: publicData.distric_name,
        imagem_perfil_link: publicData.profile_Picture_Link,
        descricao: publicData.description,
        accountType: 'PersonAccount' as const,
      } as UserCPF
    } else {
      const publicData = accountData.business_Account_Data.public_Data
      account = {
        nome: publicData.username,
        email: publicData.email,
        telefone: '',
        cnpj: publicData.cnpj,
        bairro_criacao: publicData.distric_name,
        imagem_perfil_link: publicData.profile_Picture_Link,
        descricao: publicData.description,
        accountType: 'BusinessAccount' as const,
      } as UserCNPJ
    }
    console.log(account)
    return { account }
  } catch (e) {
    console.error('Erro ao buscar conta: ', e)
    throw e
  }
}
