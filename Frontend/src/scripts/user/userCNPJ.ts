export interface UserCNPJ {
  nome: string
  cnpj: string
  telefone: string
  email: string
  senha: string
  descrição: string
  imagem_perfil_link: string
  data_criação: string
  slot_anuncio_quantidade: number
  bairro_criacao: string,
  bairro_id: number
}

export const registrarContaCNPJ = async (userData: UserCNPJ) => {
  console.log(userData)

  try {
    console.log(`Usuario registrado no sistema: ${userData.nome}`)
  } catch (e) {
    console.error('Erro ao tentar criar conta: ', e)
  }
}
