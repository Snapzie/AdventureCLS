package org.combinators.guidemo;

import javax.swing.*;
import java.awt.*;

public class CustomerForm extends JFrame {
    public JButton orderButton;
    public String selectedOrder;

    public CustomerForm() {
        super();
        FlowLayout layout = new FlowLayout();
        layout.setAlignment(FlowLayout.TRAILING);
        this.setLayout(layout);

        initComponents();

        this.pack();
        this.setVisible(true);
        this.setDefaultCloseOperation(EXIT_ON_CLOSE);
    }

    public void initComponents() {
        orderButton = new JButton();
        orderButton.setText("Order");
        orderButton.addActionListener(e ->
            JOptionPane.showMessageDialog(orderButton, String.format("You ordered: %s", selectedOrder))
        );
        this.add(orderButton);
    }

    public static void main(String[] args) {
        CustomerForm form = new CustomerForm();
    }
}