#pragma once

#include "gate-logic/gate_logic.hpp"
#include "circuits/circuit_component.hpp"
#include <vector>

class MinimalGate : public CircuitComponent {
    public:
    GateLogic* logic;

    MinimalGate(GateLogic* gl, std::vector<int>* inps) : CircuitComponent(inps, new std::vector<int>{ 1 }) {
        this->logic = gl;
    }

    void PerformLogic() {
        this->outputs.at(0)->SetValues(logic->PerformGateLogic(this->inputs));
    }
};