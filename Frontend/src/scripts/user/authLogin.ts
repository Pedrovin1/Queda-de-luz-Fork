import { type UserLogin } from "./userGeneric";

const API_BANCO_DE_DADOS = 'http://localhost:5176/accounts/login'

export const fetchLogin = async (credentials: UserLogin) => {
    const payload = {
        Username: credentials.email,
        Password: credentials.senha,
    }

    try{
        const response = await fetch(API_BANCO_DE_DADOS, {
            method: 'GET',
            headers: {
                'Content-type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(payload)
        })

        if(!response.ok) {
            throw new Error(`Falha na autenticação de login: ${response.status}`)
        }

        return await response.json()
    }catch(e){
        console.error("Erro ao tentar conexão com banco de dados: ", e)
        throw e
    }
}