# Sistema de Comandos de Guild - OpenCoreMMO

Este documento descreve os comandos de guild implementados no OpenCoreMMO, baseados no sistema do ForgottenServer.

## Comandos Implementados

### 1. Criação de Guild

**Comando:** `!createguild "Nome da Guild"` ou `!foundguild "Nome da Guild"`

**Descrição:** Cria uma nova guild com o jogador como líder.

**Requisitos:**
- Nível mínimo: 20
- Não pode estar em uma guild
- Nome da guild deve ter entre 3 e 50 caracteres
- Nome da guild deve ser único

**Exemplo:**
```
!createguild "Knights of Honor"
!foundguild "Dark Brotherhood"
```

### 2. Convite para Guild

**Comando:** `!guildinvite "Nome do Jogador"` ou `!invite "Nome do Jogador"`

**Descrição:** Convida um jogador para a guild.

**Requisitos:**
- Deve estar em uma guild
- Deve ser líder (rank 1) ou vice-líder (rank 2)

**Status:** Parcialmente implementado (necessita integração com sistema de jogadores)

**Exemplo:**
```
!guildinvite "PlayerName"
!invite "PlayerName"
```

### 3. Sair da Guild

**Comando:** `!guildleave` ou `!leave`

**Descrição:** Remove o jogador da guild atual.

**Requisitos:**
- Deve estar em uma guild
- Não pode ser o líder da guild (deve transferir liderança primeiro)

**Exemplo:**
```
!guildleave
!leave
```

### 4. Alterar MOTD da Guild

**Comando:** `!guildmotd "Nova mensagem"` ou `!motd "Nova mensagem"`

**Descrição:** Altera a mensagem do dia (MOTD) da guild.

**Requisitos:**
- Deve estar em uma guild
- Deve ser líder (rank 1) ou vice-líder (rank 2)
- Mensagem não pode exceder 255 caracteres

**Exemplo:**
```
!guildmotd "Welcome to our guild! Be respectful and have fun!"
!motd "Guild meeting tomorrow at 8 PM"
!motd ""  // Para limpar a MOTD
```

### 5. Declarar Guerra de Guild

**Comando:** `!guildwar "Nome da Guild Inimiga"` ou `!war "Nome da Guild Inimiga"`

**Descrição:** Declara guerra contra outra guild.

**Requisitos:**
- Deve estar em uma guild
- Deve ser líder (rank 1)
- Guild inimiga deve existir
- Não pode declarar guerra contra a própria guild

**Exemplo:**
```
!guildwar "Enemy Guild"
!war "Rival Knights"
```

### 6. Encerrar Guerra de Guild

**Comando:** `!endwar "Nome da Guild Inimiga"` ou `!peace "Nome da Guild Inimiga"`

**Descrição:** Encerra uma guerra entre guilds.

**Requisitos:**
- Deve estar em uma guild
- Deve ser líder (rank 1)
- Deve haver uma guerra ativa com a guild especificada

**Exemplo:**
```
!endwar "Enemy Guild"
!peace "Former Enemies"
```

### 7. Informações da Guild

**Comando:** `!guildinfo` ou `!ginfo`

**Descrição:** Mostra informações sobre a guild atual.

**Requisitos:**
- Deve estar em uma guild

**Exemplo:**
```
!guildinfo
!ginfo
```

**Saída:**
```
Guild: Knights of Honor
Members: 15
Created: 2025-08-07
MOTD: Welcome to our guild!
```

### 8. Membros da Guild

**Comando:** `!guildmembers` ou `!gmembers`

**Descrição:** Lista os membros da guild.

**Requisitos:**
- Deve estar em uma guild

**Status:** Não implementado (aguardando sistema de listagem de membros)

**Exemplo:**
```
!guildmembers
!gmembers
```

## Sistema de Ranks

O sistema implementa ranks padrão compatíveis com o ForgottenServer:

1. **Leader (Líder)** - Rank 1
   - Pode criar e dissolver a guild
   - Pode declarar e encerrar guerras
   - Pode convidar e expulsar membros
   - Pode alterar MOTD
   - Pode gerenciar ranks

2. **Vice-Leader (Vice-Líder)** - Rank 2
   - Pode convidar membros
   - Pode alterar MOTD
   - Não pode declarar guerras

3. **Member (Membro)** - Rank 3
   - Membro regular da guild
   - Acesso apenas a funcionalidades básicas

## Integração Técnica

### Arquitetura

- **GuildCommandManager**: Gerencia todos os comandos de guild
- **PlayerSayCommand**: Intercepta mensagens e processa comandos
- **GuildService**: Serviços de backend para operações de guild
- **Injeção de Dependência**: Configurada em `CommandInjection.cs`

### Processamento de Comandos

1. Player digita comando começando com `!`
2. `PlayerSayCommand` intercepta a mensagem
3. `GuildCommandManager.ProcessGuildCommand()` é chamado
4. Comando apropriado é executado
5. Resposta é enviada ao player

### Base de Dados

O sistema utiliza as seguintes entidades:
- `GuildEntity`: Informações básicas da guild
- `GuildRankEntity`: Ranks da guild
- `GuildMembershipEntity`: Membros da guild
- `GuildWarEntity`: Guerras entre guilds
- `GuildWarKillEntity`: Kills em guerras
- `GuildInviteEntity`: Convites pendentes

## Exemplos de Uso

### Criando uma Guild
```
Jogador: !createguild "Knights of Valor"
Sistema: You have successfully created the guild 'Knights of Valor'!
```

### Declarando Guerra
```
Líder: !guildwar "Enemy Forces"
Sistema: War has been declared against Enemy Forces!
```

### Alterando MOTD
```
Líder: !motd "Training session tonight at castle!"
Sistema: Guild MOTD has been changed to: Training session tonight at castle!
```

## Notas de Implementação

- Todos os comandos são **case-insensitive**
- Nomes de guild e jogadores devem ser colocados entre aspas se contiverem espaços
- O sistema é **totalmente assíncrono** para melhor performance
- Logs detalhados são mantidos para auditoria
- Compatível com o sistema de guilds do ForgottenServer

## Próximas Implementações

- Sistema de convites de guild
- Listagem de membros online
- Sistema de permissões de rank personalizadas
- Interface web para gerenciamento de guilds
- Histórico de guerras e estatísticas
