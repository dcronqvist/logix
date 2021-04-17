#pragma once

#include "circuits/circuit_component.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
#include "utils/utility.hpp"
#include "circuits/circuit_component.hpp"
#include "minimals/minimal_switch.hpp"
#include "circuits/circuit_io.hpp"

class ICInput : public CircuitInput {
    public:
    std::vector<MinimalSwitch*> switchMap;
    std::string id;

    ICInput(int bits, std::vector<MinimalSwitch*> switchMap, std::string id) : CircuitInput(bits, LogicValue_LOW) {
        this->switchMap = switchMap;
        this->id = id;
    }
};