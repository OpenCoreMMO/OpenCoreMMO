function PrettyString(tbl, indent)
	if not indent then
		indent = 0
	end
	if type(tbl) ~= "table" then
		return tostring(tbl)
	end
	local toprint = string.rep(" ", indent) .. "{\n"
	indent = indent + 2
	for k, v in pairs(tbl) do
		toprint = toprint .. string.rep(" ", indent)
		if type(k) == "number" then
			toprint = toprint .. "[" .. k .. "] = "
		elseif type(k) == "string" then
			toprint = toprint .. k .. "= "
		end
		if type(v) == "number" then
			toprint = toprint .. v .. ",\n"
		elseif type(v) == "string" then
			toprint = toprint .. '"' .. v .. '",\n'
		elseif type(v) == "table" then
			toprint = toprint .. PrettyString(v, indent + 2) .. ",\n"
		elseif type(v) == "userdata" then
			toprint = toprint .. '"' .. tostring(v) .. '",\n'
		else
			toprint = toprint .. '"' .. tostring(v) .. '",\n'
		end
	end
	toprint = toprint .. string.rep(" ", indent - 2) .. "}"
	return toprint
end

function getTibiaTimerDayOrNight()
	local light = getWorldLight()
	if light == 40 then
		return "night"
	else
		return "day"
	end
end

function getFormattedWorldTime()
	local worldTime = getWorldTime()
	local hours = math.floor(worldTime / 60)

	local minutes = worldTime % 60
	if minutes < 10 then
		minutes = "0" .. minutes
	end
	return hours .. ":" .. minutes
end