---@param logic_value logic_value
---@return logic_value
local invert_logic_value = function(logic_value)
    if logic_value == defs.logic_value.high then
        return defs.logic_value.low
    elseif logic_value == defs.logic_value.low then
        return defs.logic_value.high
    else
        return defs.logic_value.undefined
    end
end

return {
    invert_logic_value = invert_logic_value
}
