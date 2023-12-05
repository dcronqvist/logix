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
    datatype = defs.scripting_data_type.node,
    id = new_id("node_pin"),
    name = "pin",
    create_init_state = function()
        return {
            value = defs.logic_value.low
        }
    end,
    get_pin_configs = function(state)
        return {
            {
                id = "A",
                bit_width = 1,
                update_causes_evaluation = true,
                side = defs.pin_side.left,
                position = {
                    x = 0,
                    y = 1
                },
                display_pin_name = false
            }
        }
    end,
    initialize = function(state)
        return {
            {
                pin_id = "A",
                new_values = { state.value },
                occurs_in_ticks = 0
            }
        }
    end,
    evaluate = function(state, pins) end,
    get_parts = function(state, pins)
        local color = get_logic_value_color(state.value)

        return {
            part_rect({ 0, 0 }, { 2, 2 }, defs.colors.black, true),
            part_rect({ 0.1, 0.1 }, { 1.8, 1.8 }, defs.colors.white, false),

            part_rect({ 0.3, 0.3 }, { 1.4, 1.4 }, defs.colors.black, false),
            part_rect_rightclickable({ 0.35, 0.35 }, { 1.3, 1.3 }, color, false, function()
                state.value = invert_logic_value(state.value)
                return {
                    {
                        pin_id = "A",
                        new_values = { state.value },
                        occurs_in_ticks = 0
                    }
                }
            end)
        }
    end
}
