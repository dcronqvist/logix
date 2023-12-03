local nodes = {
    {
        datatype = defs.scripting_data_type.node,
        id = new_id("core1"),
        name = "big node",
        create_init_state = function()
            return {}
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
                    display_pin_name = true
                },
                {
                    id = "B",
                    bit_width = 1,
                    update_causes_evaluation = false,
                    side = defs.pin_side.right,
                    position = {
                        x = 2,
                        y = 1
                    },
                    display_pin_name = true
                }
            }
        end,
        initialize = function(state)
            return {}
        end,
        evaluate = function(state, pins)
            local a = pins:read("A", 0)

            if a == defs.logic_value.high then
                return {
                    {
                        pin_id = "B",
                        new_values = { defs.logic_value.low },
                        occurs_in_ticks = 0
                    }
                }
            elseif a == defs.logic_value.low then
                return {
                    {
                        pin_id = "B",
                        new_values = { defs.logic_value.high },
                        occurs_in_ticks = 0
                    }
                }
            else
                return {
                    {
                        pin_id = "B",
                        new_values = { defs.logic_value.undefined },
                        occurs_in_ticks = 0
                    }
                }
            end
        end,
        get_parts = function(state, pins)
            return {
                part_rect({ 0, 0 }, { 2, 2 }, defs.color.black, true),
                part_rect({ 0.1, 0.1 }, { 1.8, 1.8 }, defs.color.white, false)
            }
        end,
    }
}

table.extend(data, nodes)
