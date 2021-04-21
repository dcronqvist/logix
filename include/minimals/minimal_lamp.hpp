#pragma once

#include "circuits/circuit_component.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
#include "utils/utility.hpp"

class MinimalLamp : public CircuitComponent {
    public:
    int bits;
    std::vector<LogicValue> values;
    std::string id;

    MinimalLamp(std::string id, int bits) : CircuitComponent(new std::vector<int>{ bits }, new std::vector<int>{}) {
        this->values = {};
        for (int i = 0; i < bits; i++) {
            this->values.push_back(LogicValue_LOW);
        }
        this->id = id;
        this->bits = bits;
    }

    std::vector<LogicValue>& GetValues() {
        return this->values;
    }

    LogicValue GetValue(int bit) {
        return this->values.at(bit);
    }

    void PerformLogic() {
        this->values = this->inputs.at(0)->GetValues();
    }
};