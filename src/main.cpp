#include <iostream>
#include "display/base_window.hpp"
#include "display/logix_window.hpp"

using namespace std;

int main() {
    BaseWindow* bw = new LogiXWindow(1280, 720);
    return bw->Run();
}