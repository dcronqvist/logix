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
        this->values.clear();
        for (int i = 0; i < this->bits; i++) {
            this->values.push_back(values.at(i));
        }
    }
    void SetValues(LogicValue val) {
        this->values.clear();
        for (int i = 0; i < bits; i++) {
            this->values.push_back(val);
        }
    }

    float GetHIGHFraction() {
        float count = 0;
        for (int i = 0; i < this->values.size(); i++) {
            if (this->values.at(i) == LogicValue_HIGH) { count = count + 1.0F; }
        }
        return count / ((float)bits);
    }
};