#pragma once

#include "circuits/circuit_component.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
#include "utils/utility.hpp"

class MinimalLamp : public CircuitComponent {
    public:
    LogicValue value;
    std::string id;

    MinimalLamp(std::string id) : CircuitComponent(new std::vector<int>{ 1 }, new std::vector<int>{}) {
        value = LogicValue_LOW;
        this->id = id;
    }

    void PerformLogic() {
        this->value = this->inputs.at(0)->GetValue(0);
    }
};