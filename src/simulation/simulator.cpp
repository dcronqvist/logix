#include "simulation/simulator.hpp"

void Simulator::Simulate() {
    for (int i = 0; i < allComponents.size(); i++) {
        allComponents.at(i)->UpdateInputsAndPerformLogic();
    }
    for (int i = 0; i < allComponents.size(); i++) {
        allComponents.at(i)->UpdateOutputs();
    }
}

void Simulator::Draw(Vector2 mousePosInWorld) {
    for (int i = 0; i < allWires.size(); i++) {
        allWires.at(i)->Draw();
    }

    for (int i = 0; i < allComponents.size(); i++) {
        allComponents.at(i)->Draw(mousePosInWorld);
    }

    for (int i = 0; i < selectedComponents.size(); i++) {
        selectedComponents.at(i)->DrawSelected();
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

void Simulator::SelectAllComponentsInRectangle(Rectangle rec) {
    this->ClearSelection();
    for (int i = 0; i < allComponents.size(); i++) {
        if(CheckCollisionRecs(rec, allComponents.at(i)->box)) {
            this->SelectComponent(allComponents.at(i));
        }
    }
}

CircuitIODesc* Simulator::GetComponentInputIODescFromPos(Vector2 position) {
    for (int i = 0; i < this->allComponents.size(); i++)
    {
        DrawableComponent* dc = allComponents.at(i);
        int index = dc->GetInputIndexFromPosition(position);

        if(index != -1) {
            return new CircuitIODesc{true, dc, index};
        }
    }
    return NULL;
}

CircuitIODesc* Simulator::GetComponentOutputIODescFromPos(Vector2 position) {
    for (int i = 0; i < this->allComponents.size(); i++)
    {
        DrawableComponent* dc = allComponents.at(i);
        int index = dc->GetOutputIndexFromPosition(position);

        if(index != -1) {
            return new CircuitIODesc{false, dc, index};
        }
    }
    return NULL;
}

DrawableWire* Simulator::GetWireFromPosition(Vector2 pos) {
    for (int i = 0; i < allWires.size(); i++)
    {
        if(allWires.at(i)->IsPositionOnLine(pos)) {
            return allWires.at(i);
        }
    }
    return NULL;
}

