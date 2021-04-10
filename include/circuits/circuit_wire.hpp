#pragma once
#include "circuits/circuit_io.hpp"
#include <vector>

class CircuitWire {
private:
    int bits;
    std::vector<LogicValue> values;

public:
    CircuitWire(int bits) {
        this->bits = bits;
        this->values = {};
        for (int i = 0; i < bits; i++) {
            this->values.push_back(LogicValue_LOW);
        }
    }
    std::vector<LogicValue>& GetValues() { return this->values; }
    void SetValues(std::vector<LogicValue>& values) {
        this->values = values;
    }
    void SetValues(LogicValue val) {
        this->values.clear();
        for (int i = 0; i < bits; i++) {
            this->values.push_back(val);
        }
    }
};
