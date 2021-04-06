#pragma once

#include "drawables/drawable_component.hpp"
#include "drawables/drawable_wire.hpp"
#include "drawables/circuit_io_desc.hpp"
#include "raylib-cpp/raylib-cpp.hpp"
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

    void RemoveWire(DrawableWire* wire) {
        allWires.erase(std::find(allWires.begin(), allWires.end(), wire));
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

    void DeleteSelectedComponents();

    void SelectAllComponentsInRectangle(Rectangle rec);

    void RemoveComponent(DrawableComponent* component);

    CircuitIODesc* GetComponentInputIODescFromPos(Vector2 position);
    CircuitIODesc* GetComponentOutputIODescFromPos(Vector2 position);

    DrawableWire* GetWireFromPosition(Vector2 pos);

    void Simulate();
    void Update(Vector2 mousePosInWorld);
    void Draw(Vector2 mousePosInWorld);

    void MoveAllSelectedComponents(Vector2 vec);
    DrawableComponent* GetComponentFromPosition(Vector2 position);
};