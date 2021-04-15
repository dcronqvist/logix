#pragma once

#include "drawables/drawable_component.hpp"
#include "drawables/drawable_wire.hpp"
#include "drawables/circuit_io_desc.hpp"
#include "drawables/drawable_lamp.hpp"
#include "drawables/drawable_switch.hpp"
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

        for (int i = 0; i < selectedComponents.size(); i++) {
            if (selectedComponents.at(i) != component) {
                newSelection.push_back(selectedComponents.at(i));
            }
        }
        selectedComponents = newSelection;
    }

    bool IsSelected(DrawableComponent* component) {
        return std::find(selectedComponents.begin(), selectedComponents.end(), component) != selectedComponents.end();
    }

    void ToggleComponentSelected(DrawableComponent* component) {
        if (IsSelected(component)) {
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

    template<class T>
    std::vector<T*> GetAllSelectedOfType() {
        std::vector<T*> found = {};

        for (int i = 0; i < this->selectedComponents.size(); i++) {
            T* t = dynamic_cast<T*>(this->selectedComponents.at(i));
            if (t != NULL) {
                found.push_back(t);
            }
        }
        return found;
    }

    std::vector<DrawableComponent*> GetAllSelectedNonIOs() {
        std::vector<DrawableComponent*> found = {};

        for (int i = 0; i < this->selectedComponents.size(); i++) {
            DrawableSwitch* ds = dynamic_cast<DrawableSwitch*>(this->selectedComponents.at(i));
            DrawableLamp* dl = dynamic_cast<DrawableLamp*>(this->selectedComponents.at(i));
            if (ds == NULL && dl == NULL) {
                found.push_back(this->selectedComponents.at(i));
            }
        }
        return found;
    }
};