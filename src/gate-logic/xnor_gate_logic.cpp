#include "gate-logic/xnor_gate_logic.hpp"
#include "circuits/circuit_input.hpp"
#include <vector>

LogicValue XNORGateLogic::PerformGateLogic(std::vector<CircuitInput*> inputs) {
    int high = 0;
    for (int i = 0; i < inputs.size(); i++) {
        CircuitInput* inp = inputs.at(i);

        for (int j = 0; j < inp->bits; j++) {
            if (inp->GetValue(j) == LogicValue_HIGH) {
                high = high + 1;
            }
        }
    }

    return high % 2 == 0 ? LogicValue_HIGH : LogicValue_LOW;
}

const char* XNORGateLogic::GetLogicName() {
    return "XNOR";
}