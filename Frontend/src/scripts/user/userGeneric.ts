export interface userGeneric {
    nome: string
    telefone: string
    email: string
    senha: string
    descrição: string
    imagem_perfil_link: string
    bairro_criacao: string
    bairro_id: number
}

export interface UserCPF extends userGeneric {
    cpf:string;
    data_nascimento: string;
}
export interface UserCNPJ extends userGeneric {
    cnpj: string;
    data_criacao: string;
    slot_anuncio_quantidade: number;
}

export type UserLogin = Pick<userGeneric, 'email' | 'senha'>