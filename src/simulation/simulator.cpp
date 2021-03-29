#include "simulation/simulator.hpp"

void Simulator::Simulate() {
    for (int i = 0; i < allComponents.size(); i++) {
        allComponents.at(i)->UpdateInputsAndPerformLogic();
    }
    for (int i = 0; i < allComponents.size(); i++) {
        allComponents.at(i)->UpdateOutputs();
    }
}

void Simulator::Draw() {
    for (int i = 0; i < allWires.size(); i++) {
        allWires.at(i)->Draw();
    }

    for (int i = 0; i < allComponents.size(); i++) {
        allComponents.at(i)->Draw();
    }
}


void Simulator::MoveAllSelectedComponents(Vector2 vec) {
    for (int i = 0; i < this->selectedComponents.size(); i++) {
        DrawableComponent* dc = this->selectedComponents.at(i);
        dc->position = dc->position + vec;
    }
}

DrawableComponent* Simulator::GetComponentFromPosition(Vector2 position) {
    for (int i = 0; i < this->allComponents.size(); i++) {
        DrawableComponent* dc = this->allComponents.at(i);
        if (CheckCollisionPointRec(position, dc->box)) {
            return dc;
        }
    }
    return NULL;
}

