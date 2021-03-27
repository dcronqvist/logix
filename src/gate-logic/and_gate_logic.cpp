#include "gate-logic/and_gate_logic.hpp"
#include "circuits/circuit_input.hpp"
#include <vector>

LogicValue ANDGateLogic::PerformGateLogic(std::vector<CircuitInput*> inputs) {
    for (int i = 0; i < inputs.size(); i++)     {
        if (inputs.at(i)->GetValue() == LogicValue_LOW) { return LogicValue_LOW; }
    }
    return LogicValue_HIGH;
}