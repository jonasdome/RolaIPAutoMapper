# RolaIPAutoMapper

Essa aplicação tem como objetivo criar um SOCKS5 com proxy residencial na América Latina para usuários que estão fora da LATAM e estão recebendo a mensagem "Desconectado do Servidor".

# RolaIPAutoMapper depende de outros dois softwares:

- [tun2socks por xjasonlyu](https://github.com/xjasonlyu/tun2socks) — Cria uma rede virtual que redireciona o tráfego para um proxy usando SOCKS5.  
- [Wintun](https://www.wintun.net/) — Utilizado pelo tun2socks.

# *Por favor, siga o processo de instalação do tun2socks [aqui](https://github.com/xjasonlyu/tun2socks/wiki/Examples)

# O que é necessário para rodar o RolaIPAutoMapper?

É necessário instalar o tun2socks e possuir um proxy residencial na América Latina. Eu estou utilizando um IP brasileiro do serviço cheap-proxy.

# Como o RolaIPAutoMapper consegue resolver o problema de conexão?

O RolaIPAutoMapper utiliza o tun2socks para se comunicar com os servidores da LATAM através de um proxy residencial, fazendo com que o acesso aparente vir de dentro da América Latina.

# Procedimento para logar

- Execute como administrador, ou não será possível criar uma interface de rede no Windows.
- Preencha o IP, a porta, o usuário e a senha do seu proxy.
- Escolha se deseja salvar essas informações. Caso sim, a aplicação irá gerar um arquivo `userConfig.json` na raiz do programa.
- Aguarde a criação da rede virtual; ela aparecerá como "Ragnarok" em *Painel de Controle > Rede e Internet > Central de Rede e Compartilhamento > Alterar as configurações do adaptador*.
- Aguarde a resolução dos domínios utilizados pelo Rola.
- Após isso, você pode logar normalmente no Ragnarok.
