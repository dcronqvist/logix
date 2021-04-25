#include "gate-logic/xor_gate_logic.hpp"
#include "circuits/circuit_input.hpp"
#include <vector>

LogicValue XORGateLogic::PerformGateLogic(std::vector<CircuitInput*> inputs) {
    int high = 0;
    for (int i = 0; i < inputs.size(); i++) {
        CircuitInput* inp = inputs.at(i);

        for (int j = 0; j < inp->bits; j++) {
            if (inp->GetValue(j) == LogicValue_HIGH) {
                high = high + 1;
            }
        }
    }

    return high % 2 == 0 ? LogicValue_LOW : LogicValue_HIGH;
}

const char* XORGateLogic::GetLogicName() {
    return "XOR";
}