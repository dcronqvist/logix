#pragma once
#include <vector>

enum LogicValue {
    LogicValue_LOW = 0,
    LogicValue_HIGH = 1,
};

class CircuitIO {
    private:
    std::vector<LogicValue> values;
    public:
    int bits;

    public:
    CircuitIO(int bits, LogicValue initialValue) {
        this->values = {};
        this->bits = bits;
        for (int i = 0; i < bits; i++) {
            this->values.push_back(initialValue);
        }
    }
    std::vector<LogicValue>& GetValues() { return this->values; }
    LogicValue GetValue(int bit) { return this->values.at(bit); }
    void SetValues(std::vector<LogicValue>& values) {
        for (int i = 0; i < this->bits; i++) {
            this->values.push_back(values.at(i));
        }
    }
    void SetValues(LogicValue val) {
        for (int i = 0; i < bits; i++) {
            this->values.at(i) = val;
        }
    }
};