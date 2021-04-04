#pragma once

#include "drawables/drawable_component.hpp"
#include "drawables/drawable_wire.hpp"
#include <vector>
#include <algorithm>

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

    void DeselectComponent(DrawableComponent* component) {
        std::vector<DrawableComponent*> newSelection;

        for (int i = 0; i < selectedComponents.size(); i++)
        {
            if(selectedComponents.at(i) != component) {
                newSelection.push_back(selectedComponents.at(i));
            }
        }
        selectedComponents = newSelection;
    }

    bool IsSelected(DrawableComponent* component) {
        return std::find(selectedComponents.begin(), selectedComponents.end(), component) != selectedComponents.end();
    }

    void ToggleComponentSelected(DrawableComponent* component) {
        if(IsSelected(component)) {
            DeselectComponent(component);
        }
        else {
            SelectComponent(component);
        }
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