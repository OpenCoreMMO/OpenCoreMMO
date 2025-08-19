-- Teste básico para a função Guild()
print("Testando a função Guild...")

local guildName = "TestGuild"
print("Tentando criar guild com nome: " .. guildName)

local guild = Guild(guildName)
if guild then
    print("Guild criada com sucesso!")
    print("Nome da guild: " .. guild:getName())
else
    print("Falha ao criar guild")
end
