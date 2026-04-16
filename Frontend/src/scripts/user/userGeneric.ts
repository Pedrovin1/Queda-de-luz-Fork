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
  accountType?: 'PersonAccount'
  cpf: string
  data_nascimento: string
}
export interface UserCNPJ extends UserGeneric {
  accountType?: 'BusinessAccount'
  cnpj: string
  data_criacao: string
  slot_anuncio_quantidade: number
}

export type UserAccount = UserCPF | UserCNPJ

export type UserLogin = Pick<UserGeneric, 'nome' | 'senha'>
