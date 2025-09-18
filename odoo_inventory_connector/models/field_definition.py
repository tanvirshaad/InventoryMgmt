# -*- coding: utf-8 -*-

from odoo import api, fields, models, _

class InventoryConnectorFieldDefinition(models.Model):
    _name = 'inventory.connector.field.definition'
    _description = 'Inventory Field Definition'
    _order = 'sequence, id'

    name = fields.Char('Field Name', required=True)
    field_type = fields.Selection([
        ('text', 'Text'),
        ('multiline', 'Multiline Text'),
        ('numeric', 'Numeric'),
        ('boolean', 'Boolean'),
        ('document', 'Document')
    ], string='Field Type', required=True, default='text')
    description = fields.Text('Description')
    show_in_table = fields.Boolean('Show in Table View', default=True)
    sequence = fields.Integer('Sequence', default=10)
    inventory_id = fields.Many2one('inventory.connector.inventory', string='Inventory', required=True, ondelete='cascade')
    
    # Numeric field specific attributes
    min_value = fields.Float('Minimum Value', digits=(16, 6))
    max_value = fields.Float('Maximum Value', digits=(16, 6))
    is_integer = fields.Boolean('Integer Only')
    
    _sql_constraints = [
        ('inventory_name_unique', 'UNIQUE(inventory_id, name)', 'Field names must be unique within an inventory')
    ]
    
    def name_get(self):
        result = []
        for record in self:
            result.append((record.id, f"{record.name} ({dict(self._fields['field_type'].selection).get(record.field_type)})"))
        return result