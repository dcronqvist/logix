#include "gate-logic/or_gate_logic.hpp"
#include "circuits/circuit_input.hpp"
#include <vector>

LogicValue ORGateLogic::PerformGateLogic(std::vector<CircuitInput*> inputs) {
    for (int i = 0; i < inputs.size(); i++) {
        CircuitInput* inp = inputs.at(i);

        for (int j = 0; j < inp->bits; j++) {
            if (inp->GetValue(j) == LogicValue_HIGH) { return LogicValue_HIGH; }
        }
    }
    return LogicValue_LOW;
}

const char* ORGateLogic::GetLogicName() {
    return "OR";
}