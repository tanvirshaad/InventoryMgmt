from odoo import models, fields, api

class FieldValue(models.Model):
    _name = 'inventory.connector.field.value'
    _description = 'Field Value for Inventory Item'
    
    item_id = fields.Many2one('inventory.connector.item', string='Item', required=True, ondelete='cascade')
    field_name = fields.Char(string='Field Name', required=True)
    
    # Field type
    field_type = fields.Selection([
        ('text', 'Text'),
        ('multiline', 'Multiline Text'),
        ('numeric', 'Numeric'),
        ('boolean', 'Boolean'),
        ('document', 'Document')
    ], string='Field Type', required=True)
    
    # Different value types based on field type
    text_value = fields.Text(string='Text Value')
    numeric_value = fields.Float(string='Numeric Value')
    boolean_value = fields.Boolean(string='Boolean Value')
    
    # Display value for list views
    display_value = fields.Char(string='Value', compute='_compute_display_value', store=True)
    
    @api.depends('field_type', 'text_value', 'numeric_value', 'boolean_value')
    def _compute_display_value(self):
        for record in self:
            if record.field_type == 'numeric':
                record.display_value = str(record.numeric_value)
            elif record.field_type == 'boolean':
                record.display_value = 'Yes' if record.boolean_value else 'No'
            else:
                record.display_value = record.text_value or ''