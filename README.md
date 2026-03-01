# Queda-de-luz

Repositório para o projeto Queda-de-luz

## Objetivo

Providenciar uma plataforma de monitoramento de energia elétrica, iniciamente apenas com o foco em Porto Alegre. O sistema consolida dodos em tempo real através de duas frentes:
- O reporte colaborativo dos usuários;
- web scraping de informações em redes sociais, portais de notícias e canais oficias da concessionária.
Sendo assim, o intuito é centralizar e informar sobre quedas de luz programadas e imprevistas de bairros da cidade.

## Ferramentas

![GitHub](https://img.shields.io/badge/GitHub-%23121011.svg?logo=github&logoColor=white)
![Git](https://img.shields.io/badge/Git-F05032?logo=git&logoColor=fff)

![JavaScript](https://img.shields.io/badge/JavaScript-F7DF1E?logo=javascript&logoColor=000)
![Static Badge](https://img.shields.io/badge/v8%2013.6.233.8-yellow)

![NodeJS](https://img.shields.io/badge/Node.js-6DA55F?logo=node.js&logoColor=white)
![Static Badge](https://img.shields.io/badge/v22.14.0-green)

![Vue](https://img.shields.io/badge/vuejs-%2335495e.svg?style=for-the-badge&logo=vuedotjs&logoColor=%234FC08D)
![Static Badge](https://img.shields.io/badge/v3.5.29-purple)

![Sass](https://img.shields.io/badge/Sass-000?style=for-the-badge&logo=sass)
![Static Badge](https://img.shields.io/badge/v1.97.3-pink)

## Instalação

Siga os passos para realizar a instalação dos processos do BackEnd e FrontEnd:

Para o gerenciamento dos pacotes do FrontEnd é preciso instalar o Node.js na sua maquina:👉 [Downlaod do NodeJS](https://nodejs.org/pt).

Uma chave de API do Google Maps: 👉[Pegue a sua API aqui](https://mapsplatform.google.com/lp/maps-apis/)

```
git clone https://github.com/NtanBraga/Queda-de-luz.git

cd Frontend
```
Adicione a sua API no arquivo .env
```
# Adicione a sua API do google maps aqui
VITE_GOOGLE_MAPS_API_KEY=your_google_maps_api_key_here
```
Instale as dependencias do projeto

```
npm install
```
E para executar o site, apenas digite este comando

```
npm start
```
