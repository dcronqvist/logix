#pragma once

#include "circuits/circuit_input.hpp"
#include "circuits/circuit_output.hpp"
#include "circuits/circuit_io.hpp"
#include "circuits/circuit_wire.hpp"
#include "circuits/circuit_component.hpp"
#include <vector>

class LogicGate : public CircuitComponent {
    public:
    LogicGate(int inputAmount) : CircuitComponent(inputAmount, 1) {}

    protected:
    void SetLow() {
        this->outputs.at(0)->SetValue(LogicValue_LOW);
    }

    void SetHigh() {
        this->outputs.at(0)->SetValue(LogicValue_HIGH);
    }
};