#!/usr/bin/env python3
import json

# Script para verificar se a porta 1209 tem transformto definido
with open("/Users/brewertonsantos/dev/opencoremmo/server/data/items/items.json", "r") as f:
    data = json.load(f)

# Verificar porta 1209
for item in data:
    if isinstance(item, dict) and item.get('id') == '1209':
        print("Porta ID 1209:")
        print(json.dumps(item, indent=2))
        
        # Verificar se tem transformto nos atributos
        attributes = item.get('attributes', [])
        transformto_found = False
        for attr in attributes:
            if attr.get('key') == 'transformto':
                print(f"\nTransformTo encontrado: {attr.get('value')}")
                transformto_found = True
                break
        
        if not transformto_found:
            print("\nNENHUM transformto encontrado!")
        break
else:
    print("Porta 1209 NÃO ENCONTRADA!")

# Verificar quantas portas têm transformto
doors_with_transformto = 0
doors_without_transformto = 0

for item in data:
    if isinstance(item, dict) and isinstance(item.get('attributes'), list):
        # Verificar se é porta
        is_door = False
        has_transformto = False
        
        for attr in item.get('attributes', []):
            if attr.get('key') == 'type' and attr.get('value') == 'door':
                is_door = True
            if attr.get('key') == 'transformto':
                has_transformto = True
        
        if is_door:
            if has_transformto:
                doors_with_transformto += 1
            else:
                doors_without_transformto += 1

print(f"\nResumo:")
print(f"Portas COM transformto: {doors_with_transformto}")
print(f"Portas SEM transformto: {doors_without_transformto}")
