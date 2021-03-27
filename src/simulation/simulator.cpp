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