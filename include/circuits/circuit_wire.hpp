#pragma once
#include "circuits/circuit_io.hpp"

class CircuitWire {
private:
    LogicValue value;

public:
    CircuitWire() { value = LogicValue_LOW; }
    LogicValue GetValue() { return value; }
    void SetValue(LogicValue val) { value = val; }
};
