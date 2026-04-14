export interface UserGeneric {
  nome: string
  telefone: string
  email: string
  senha: string
  descricao: string
  imagem_perfil_link: string
  bairro_criacao: string
  bairro_id: number
}

export interface UserCPF extends UserGeneric {
  cpf: string
  data_nascimento: string
}
export interface UserCNPJ extends UserGeneric {
  cnpj: string
  data_criacao: string
  slot_anuncio_quantidade: number
}

export type UserLogin = Pick<UserGeneric, 'email' | 'senha'>
