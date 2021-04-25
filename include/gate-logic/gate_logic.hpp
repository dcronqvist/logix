#pragma once

#include "circuits/circuit_input.hpp"
#include <vector>

class GateLogic {
public:
    virtual LogicValue PerformGateLogic(std::vector<CircuitInput*> inputs) = 0;
    virtual const char* GetLogicName() = 0;
};