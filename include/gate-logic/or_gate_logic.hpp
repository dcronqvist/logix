#pragma once

#include "gate-logic/gate_logic.hpp"
#include "circuits/circuit_input.hpp"
#include <vector>

class ORGateLogic : public GateLogic {
    public:
    LogicValue PerformGateLogic(std::vector<CircuitInput*> inputs);
    const char* GetLogicName();
};