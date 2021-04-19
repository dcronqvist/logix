#pragma once

#include "circuits/circuit_component.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
#include "utils/utility.hpp"
#include "circuits/circuit_component.hpp"
#include "minimals/minimal_switch.hpp"
#include "circuits/circuit_io.hpp"

struct BitPointer {
    MinimalSwitch* sw;
    int toBit;
    int fromBit;
};

class ICInput : public CircuitInput {
public:
    std::vector<BitPointer> switchMap;
    std::string id;

    ICInput(int bits, std::vector<BitPointer> switchMap, std::string id) : CircuitInput(bits, LogicValue_LOW) {
        this->switchMap = switchMap;
        this->id = id;
    }
};