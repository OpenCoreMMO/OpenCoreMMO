-- local talkAction = TalkAction("/benchmark")

-- function talkAction.onSay(player, words, param)
-- 	do
--     local units = {
--         ['seconds'] = 1,
--         ['milliseconds'] = 1000,
--         ['microseconds'] = 1000000,
--         ['nanoseconds'] = 1000000000
--     }

--     function benchmark(unit, decPlaces, n, f, ...)
--         local elapsed = 0
--         local multiplier = units[unit]
--         for i = 1, n do
--             local now = os.clock()
--             f(...)
--             elapsed = elapsed + (os.clock() - now)
--         end
--         print(
--             string.format('Benchmark results: %d calls | %.'.. decPlaces ..'f %s elapsed | %.'.. decPlaces ..'f %s avg execution time.', n, elapsed * multiplier, unit, (elapsed / n) * multiplier, unit))
--             end
--         end

--     function test(n)
--         local x = "x";
--         local str = "Minha string" .. x
--         --local result = 10 + 20;
--         --    print(n)
--         --local t = {}
--         --for i = 1, n do
--             --[i] = i
--             --local str = "Minha string" .. x
--         --    print(i)
--         --end
--     end

--     benchmark('milliseconds', 2, 500000, test, 1) -- Benchmark results: 500 function calls | 254.96 milliseconds elapsed | 0.51 milliseconds avg execution time.
-- end

-- talkAction:separator(" ")
-- talkAction:register()
