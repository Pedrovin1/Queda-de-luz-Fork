import { type UserCPF } from './userGeneric'

const API_BANCO_DE_DADOS = 'http://localhost:5176/accounts'

export const registrarContaCPF = async (userData: UserCPF) => {
  console.log(userData)

  const payload = {
    Username: userData.nome,
    Unhashed_Password: userData.senha,
    Email: userData.email,
    District_Id: userData.bairro_id,
    Person_Details: {
      Birthday: userData.data_nascimento,
    },
    Business_Details: null,
  }

  try {
    const response = await fetch(API_BANCO_DE_DADOS, {
      method: 'POST',
      headers: { 'Content-type': 'application/json' },
      body: JSON.stringify(payload),
    })

    if (!response.ok) {
      const text = await response.text()
      throw new Error(
        `Erro ao enviar conta registrada para o banco de dados: ${response.status} -${text}`,
      )
    }
    console.log(`Usuario registrado no sistema: ${payload.Username}`)
    return await response.json()
  } catch (e) {
    console.error('Erro ao tentar criar conta: ', e)
    throw e
  }
}

export const logarContaCPF = async (username: string): Promise<UserCPF> => {
  try {
    const response = await fetch(`${API_BANCO_DE_DADOS}/${username}`)

    if(!response.ok){
      throw new Error(`Erro ao buscar dados do CPF: ${response.status}`)
    }

    const data = await response.json()


    return {
      nome: data.Username,
      email: data.Email,
      senha: '',
      telefone: data.Telefone  || '', //Refazer quando inserido telefone pelo backend
      descricao: '',
      imagem_perfil_link: '',
      bairro_id: Number(data.District_Id),
      bairro_criacao: '',
      cpf: data.CPF,
      data_nascimento: new Date(data.UTC_datetime_creation).toLocaleDateString('pt-BR')
      
    }

  } catch (e) {
    console.error('Erro ao tentar GET da conta: ', e)
    throw e
  }
}
