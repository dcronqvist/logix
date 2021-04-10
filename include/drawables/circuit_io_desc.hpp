#pragma once
#include "drawables/drawable_component.hpp"

class CircuitIODesc {
public:
    bool isInput;
    DrawableComponent* component;
    int index;
    int bits;
};