# RolaIPAutoMapper

RolaIPAutoMapper utiliza o tun2socks para redirecionar o tráfego do Ragnarok Online através de um proxy e resolve automaticamente todos os IPs do Rola, permitindo que jogadores fora da América Latina consigam se conectar aos servidores do jogo.

# RolaIPAutoMapper depende de outros dois softwares:

- [tun2socks por xjasonlyu](https://github.com/xjasonlyu/tun2socks) — Cria uma rede virtual que redireciona o tráfego para um proxy usando SOCKS5.  
- [Wintun](https://www.wintun.net/) — Utilizado pelo tun2socks.

# tun2socks Instalação
- Faça o download da versão mais recente disponível [aqui](https://github.com/xjasonlyu/tun2socks/releases)
- Extraia o conteúdo do arquivo ZIP em uma pasta de sua escolha.
- Faça o download do Wintun [aqui](https://www.wintun.net/builds/wintun-0.14.1.zip) e extraia o conteúdo na mesma pasta do tun2socks.
- Adicione o caminho da pasta onde o tun2socks e o Wintun foram extraídos à variável de ambiente `PATH` do Windows. Isso pode ser feito através das configurações avançadas do sistema.
- Abra um novo terminal e execute o comando tun2socks; ele deve retornar algo parecido com isto:
![image](https://github.com/user-attachments/assets/6c69c75a-a798-491d-acbe-e5dff03d91bb)


# *Para mais exemplos de como tun2socks funciona, você pode verificar [aqui](https://github.com/xjasonlyu/tun2socks/wiki/Examples)

# Como usar

- Após instalar o tun2socks e o Wintun, execute o RolaIPAutoMapper como administrador, ou não será possível criar uma interface de rede no Windows.
- Preencha o IP, a porta, o usuário e a senha do seu proxy, e escolha a interface de rede que está conectada à internet.
- Escolha se deseja salvar essas informações. Caso sim, a aplicação irá gerar um arquivo `userConfig.json` na raiz do programa.
- Aguarde a criação da rede virtual; ela aparecerá como "Ragnarok" em *Painel de Controle > Rede e Internet > Central de Rede e Compartilhamento > Alterar as configurações do adaptador*.
- Aguarde a resolução dos domínios utilizados pelo Rola.
- Após isso, você pode logar normalmente no Ragnarok.
![image](https://github.com/user-attachments/assets/d7b7a5f9-a356-4871-b983-4249c9034d62)

# O que é necessário para rodar o RolaIPAutoMapper?

É necessário instalar o tun2socks e possuir um proxy residencial na América Latina. Eu estou utilizando um IP brasileiro do serviço [cheap-proxy](https://www.proxy-cheap.com/services/static-residential-proxies).

# Como o RolaIPAutoMapper consegue resolver o problema de conexão?

O RolaIPAutoMapper utiliza o tun2socks para se comunicar com os servidores da LATAM através de um proxy residencial, fazendo com que o acesso aparente vir de dentro da América Latina.




