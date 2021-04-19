#pragma once

#include "circuits/circuit_component.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
#include "utils/utility.hpp"
#include "circuits/circuit_component.hpp"

class MinimalSwitch : public CircuitComponent {
public:
    int bits;
    std::vector<LogicValue> values;
    std::string id;

    MinimalSwitch(int bits, std::string id) : CircuitComponent(new std::vector<int>{}, new std::vector<int>{ bits }) {
        this->bits = bits;
        this->values = {};
        this->id = id;
        for (int i = 0; i < bits; i++) {
            this->values.push_back(LogicValue_LOW);
        }
    }

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

    void SetValue(int bit, LogicValue val) {
        this->values.at(bit) = val;
    }

    void PerformLogic() {
        this->outputs.at(0)->SetValues(this->values);
    }
};