#include "simulation/simulator.hpp"
#include "raylib-cpp/raylib-cpp.hpp"

void Simulator::Simulate() {
    for (int i = 0; i < allComponents.size(); i++) {
        allComponents.at(i)->UpdateInputsAndPerformLogic();
    }
    for (int i = 0; i < allComponents.size(); i++) {
        allComponents.at(i)->UpdateOutputs();
    }
}

void Simulator::Update(Vector2 mousePosInWorld) {
    for (int i = 0; i < allComponents.size(); i++)
    {
        allComponents.at(i)->Update(mousePosInWorld);
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

void Simulator::DeleteSelectedComponents() {
    if (selectedComponents.size() == allComponents.size()) {
        selectedComponents = {};
        allComponents = {};
        allWires = {};
    }

    for (int i = 0; i < this->selectedComponents.size(); i++)
    {
        this->RemoveComponent(selectedComponents.at(i));
    }
    ClearSelection();
}

void Simulator::RemoveComponent(DrawableComponent* component) {
    std::vector<DrawableComponent*> comps;

    for (int i = 0; i < allComponents.size(); i++) {
        if (allComponents.at(i) != component) {
            comps.push_back(allComponents.at(i));
            continue;
        }

        DrawableComponent* dc = allComponents.at(i);
        // TODO: do everything that must be done when deleting a component.
        for (int j = 0; j < dc->inputs.size(); j++)
        {
            CircuitInput* ci = dc->inputs.at(j);

            if(ci->HasSignal()) {
                DrawableWire* wire = (DrawableWire*)(ci->GetSignal());
                wire->from->RemoveOutputWire(wire->fromIndex, wire);
                this->RemoveWire(wire);
            }
        }

        for (int j = 0; j < dc->outputs.size(); j++) {
            CircuitOutput* co = dc->outputs.at(j);

            if(co->HasAnySignal()) {
                for (int k = 0; k < co->GetSignals().size(); k++)
                {
                    DrawableWire* wire = (DrawableWire*)(co->GetSignals().at(k));
                    wire->to->RemoveInputWire(wire->toIndex);
                    this->RemoveWire(wire);
                }          
            }
        }    
    }
    allComponents = comps;
}

