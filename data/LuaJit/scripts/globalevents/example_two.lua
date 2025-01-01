local Example_Two = GlobalEvent("Example two")
function Example_Two.onThink(interval)
	logger.info('GlobalEvent Example_Two: onThink')
	return true
end

Example_Two:interval(10000) -- 10 seconds interval
Example_Two:register()