const API_BANCO_DE_DADOS = 'http://localhost:5176/accounts'

export interface UserCPF {
  nome: string
  cpf: string
  telefone: string
  email: string
  senha: string
  descrição: string
  imagem_perfil_link: string
  data_nascimento: string
  bairro_criacao: string
}

export const registrarContaCPF = async (userData: UserCPF) => {
  console.log(userData)

  const payload = {
    Username: userData.nome,
    Unhashed_Password: userData.senha,
    Email: userData.email,
    District_Id: 1,
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
