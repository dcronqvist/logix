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
        for (int i = 0; i < bits; i++) {
            this->values.at(i) = LogicValue_LOW;
        }
    }
    std::vector<LogicValue>& GetValues() { return this->values; }
    void SetValues(std::vector<LogicValue>& values) {
        for (int i = 0; i < this->bits; i++) {
            this->values.at(i) = values.at(i);
        }
    }
    void SetValues(LogicValue val) {
        for (int i = 0; i < bits; i++) {
            this->values.at(i) = val;
        }
    }
};
