#pragma once

#include "drawables/drawable_component.hpp"
#include "drawables/drawable_wire.hpp"
#include <vector>

class Simulator {
public:
    std::vector<DrawableComponent*> allComponents;
    std::vector<DrawableWire*> allWires;
    std::vector<DrawableComponent*> selectedComponents;

    Simulator() {

    }

    void AddWire(DrawableWire* wire) {
        allWires.push_back(wire);
    }

    void AddComponent(DrawableComponent* component) {
        allComponents.push_back(component);
    }

    void SelectComponent(DrawableComponent* component) {
        selectedComponents.push_back(component);
    }

    void ClearSelection() {
        selectedComponents = {};
    }

    void RemoveComponent(DrawableComponent* component) {
        std::vector<DrawableComponent*> comps;

        for (int i = 0; i < allComponents.size(); i++) {
            if (allComponents.at(i) != component) {
                comps.push_back(allComponents.at(i));
            }
        }
        allComponents = comps;
        delete component;
    }

    void Simulate();
    void Draw();

    void MoveAllSelectedComponents(Vector2 vec);
    DrawableComponent* GetComponentFromPosition(Vector2 position);
};