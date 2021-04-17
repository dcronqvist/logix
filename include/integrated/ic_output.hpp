#pragma once

#include "circuits/circuit_component.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
#include "utils/utility.hpp"
#include "circuits/circuit_component.hpp"
#include "minimals/minimal_lamp.hpp"
#include "circuits/circuit_io.hpp"
#include "circuits/circuit_output.hpp"

class ICOutput : public CircuitOutput {
    public:
    std::vector<MinimalLamp*> lampMap;
    std::string id;

    ICOutput(int bits, std::vector<MinimalLamp*> lampMap, std::string id) : CircuitOutput(bits, LogicValue_LOW) {
        this->lampMap = lampMap;
        this->id = id;
    }
};