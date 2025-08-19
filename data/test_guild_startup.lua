-- Teste da função Guild() no startup
print("=== TESTE DE FUNÇÃO GUILD ===")
print("Testando se Guild() está disponível...")

-- Teste 1: Verificar se Guild existe
if Guild then
    print("✓ Função Guild está disponível")
    
    -- Teste 2: Tentar criar uma guild
    local testName = "TestGuild"
    print("Tentando criar guild com nome: " .. testName)
    
    local guild = Guild(testName)
    if guild then
        print("✓ Guild criada com sucesso!")
        print("  - ID: " .. guild:getId())
        print("  - Nome: " .. guild:getName())
        print("  - MOTD: " .. guild:getMotd())
    else
        print("✗ Guild() retornou nil")
    end
else
    print("✗ Função Guild NÃO está disponível")
end

print("=== FIM DO TESTE ===")
