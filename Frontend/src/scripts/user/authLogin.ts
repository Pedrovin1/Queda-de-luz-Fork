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

const GetLoginDataByToken = async (token: string) => {
  try {
    const response = await fetch(`${API_BANCO_DE_DADOS}/login`, {
      method: 'GET',
      headers: {
        Authorization: `Bearer ${token}`,
        Accept: 'application/json',
      },
    })

    if (!response.ok) {
      throw new Error('Token invalido!')
    }

    return await response.json()
  } catch (e) {
    console.error('Erro ao tentar pegar informações do token:', e)
    throw e
  }
}

export const giveAccountInfo = async (credentials: UserLogin) => {
  const token = await fetchToken(credentials)

  const tokenData = await GetLoginDataByToken(token)

  console.log('Esse é as informações do token:', tokenData)

  try {
    const response = await fetch(`${API_BANCO_DE_DADOS}/${tokenData.account_Id}`, {
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

    const data = await response.json()

    console.log('Informações do Id da conta:', data)

    let account: UserAccount

    if (data.business_Account_Data == null) {
      const personData = data.person_Account_Data.public_Data

      account = {
        nome: personData.username,
        email: personData.email,
        telefone: '',
        data_nascimento: personData.birthday,
        descricao: personData.description,
        imagem_perfil_link: personData.profile_Picture_Link,
        bairro_criacao: personData.district_Name,
        bairro_id: personData.district_Id,
        accountType: 'PersonAccount' as const,
      } as UserCPF
    } else {
      const businessPublicData = data.business_Account_Data.public_Data
      const businessPrivateData = data.business_Account_Data.private_Data

      account = {
        nome: businessPublicData.username,
        email: businessPublicData.email,
        cnpj: businessPublicData.cnpj,
        descricao: businessPublicData.description,
        imagem_perfil_link: businessPublicData.profile_Picture_Link,
        bairro_criacao: businessPublicData.district_Name,
        bairro_id: businessPublicData.district_Id,
        slot_anuncio_quantidade: businessPrivateData.advertisement_Slots_Amount,
        accountType: 'BusinessAccount' as const,
      } as UserCNPJ
    }

    return { account }
  } catch (e) {
    console.error('Erro ao buscar conta: ', e)
    throw e
  }
}
