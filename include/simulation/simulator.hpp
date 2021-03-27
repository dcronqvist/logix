#pragma once

#include "drawables/drawable_component.hpp"
#include "drawables/drawable_wire.hpp"
#include <vector>

class Simulator {
    public:
    std::vector<DrawableComponent*> allComponents;
    std::vector<DrawableWire*> allWires;

    Simulator() {

    }

    void AddWire(DrawableWire* wire) {
        allWires.push_back(wire);
    }

    void AddComponent(DrawableComponent* component) {
        allComponents.push_back(component);
    }

    void Simulate();
    void Draw();
};